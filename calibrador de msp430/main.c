#include <msp430g2553.h>
#include "Temporizador.h"

#define TEMPO_TICK 10000
#define tp BIT3
#define tu BIT4
volatile unsigned int tick = 0;

void setup_tick(unsigned int tempo_tick);
void UART_init_1MHz(void);
void UART_init_dinamico(unsigned int delta);
unsigned char UART_read_char(void);
void UART_sendChar(char c);
void Set_DCO(unsigned int Delta);

int main(void) {
    WDTCTL = WDTPW | WDTHOLD;

    // Atraso de boot para segurança do Gravador CCS (evita erro de sincronização)
    volatile unsigned long i;
    for(i = 0; i < 20000; i++);

    P1DIR &= ~BIT5;
    P1REN &= ~(BIT5 + BIT2);
    P1DIR |= tp + tu;
    P1REN &= ~tp;
    P1OUT &= ~tp;
    P1REN |= tu;
    P1OUT |= tu;

    P1DIR |= BIT0 + BIT6;
    P1REN |= BIT0 + BIT6;
    P1OUT &= ~(BIT0 + BIT6);

    // ========================================================
    // ETAPA 1: PRÉ-CALIBRAÇÃO DE SEGURANÇA (1 MHz)
    // ========================================================
    P1OUT |= BIT6;
    Set_DCO(104); // <-- MUDOU DE 416 PARA 104 (1.000.000 / 9600)
    P1OUT &= ~BIT6;

    unsigned char dco_1mhz = DCOCTL;
    unsigned char bcs_1mhz = BCSCTL1;

    // ========================================================
    // ETAPA 2: LEITURA SEGURA (Com proteção de Reset)
    // ========================================================
    UART_init_1MHz();
    UART_sendChar('R');

    P1OUT |= BIT0;

    unsigned char sync;
    do {
        sync = UART_read_char();

        // Se o aplicativo C# abortar e mandar um 'Z' enquanto espera
        if (sync == 'Z') {
            WDTCTL = 0;
        }
    } while (sync != 0xAA);

    unsigned char byte_alto = UART_read_char();
    unsigned char byte_baixo = UART_read_char();
    unsigned int target_delta = (byte_alto << 8) | byte_baixo;

    P1OUT &= ~BIT0;

    // ========================================================
    // ETAPA 3: CALIBRAÇÃO FINAL
    // ========================================================
    P1OUT |= BIT0 + BIT6;

    Set_DCO(target_delta);

    P1OUT &= ~(BIT0 + BIT6);

    unsigned char final_dco = DCOCTL;
    unsigned char final_bcs = BCSCTL1;

    // ========================================================
    // ETAPA 4: ENVIO DOS DADOS (Volta temporariamente a 1 MHz)
    // ========================================================
    BCSCTL1 = bcs_1mhz;
    DCOCTL = dco_1mhz;

    UART_init_1MHz();

    setup_tick(TEMPO_TICK);
    __enable_interrupt();

    UART_sendChar('q');
    UART_sendChar(final_dco);
    UART_sendChar(final_bcs);

    Temporizador t_espera_tx;
    inicializa_temporizador(20, &t_espera_tx);
    while (!passou_tempo(&t_espera_tx));

    // ========================================================
    // ETAPA 5: RETORNO À FREQÜÊNCIA ESCOLHIDA E EXECUÇÃO
    // ========================================================
    BCSCTL1 = final_bcs;
    DCOCTL = final_dco;

    // Reajusta a velocidade da porta serial para a nova frequência do chip
    UART_init_dinamico(target_delta);

    Temporizador t_pisca;
    inicializa_temporizador(100, &t_pisca);

    while(1) {
        if (passou_tempo(&t_pisca)) {
            reseta_temporizador(&t_pisca);
            P1OUT ^= BIT0 + BIT6;
        }

        // Agora escuta perfeitamente o 'Z' em 8MHz, 12MHz, 16MHz, etc.
        if (IFG2 & UCA0RXIFG) {
            unsigned char comando = UCA0RXBUF;
            if (comando == 'Z') {
                WDTCTL = 0;
            }
        }
    }
}

// ========================================================
// FUNÇÕES AUXILIARES E DE CALIBRAÇÃO
// ========================================================
void Set_DCO(unsigned int Delta) {
    unsigned int Compare, Oldcapture = 0;

    P1DIR &= ~BIT1;
    P1SEL |= BIT1;
    P1SEL2 &= ~BIT1;

    TA0CCTL0 = CM_3  | CCIS_0 | SCS | CAP;
    TA0CTL = TASSEL_2 + MC_2 + TACLR;

    unsigned int tempo_inicial = TA0R;
    char e_ja_capturou_antes = 0;

    while (1) {
        if (CCIFG & TACCTL0) {
            TACCTL0 &= ~CCIFG;

            if (e_ja_capturou_antes) {
                Compare = TACCR0;
                Compare = Compare - Oldcapture;

                if (Compare >= Delta - 1 && Compare <= Delta + 1) {
                    break;
                }
                else if (Delta < Compare) {
                    DCOCTL--;
                    if (DCOCTL == 0xFF)
                        if (BCSCTL1 & 0x0f) BCSCTL1--;
                }
                else {
                    DCOCTL++;
                    if (DCOCTL == 0x00)
                        if ((BCSCTL1 & 0x0f) != 0x0f) BCSCTL1++;
                }
            }
            Oldcapture = TACCR0;
            e_ja_capturou_antes = 1;
            tempo_inicial = TA0R;
        }

        if ((unsigned int)(TA0R - tempo_inicial) > (unsigned int)10000) {
            e_ja_capturou_antes = 0;
        }
    }

    TACCTL0 = 0;
    TACTL = 0;
}

unsigned char UART_read_char(void){
    while (!(IFG2 & UCA0RXIFG));
    return UCA0RXBUF;
}

void UART_sendChar(char c){
    while (!(IFG2 & UCA0TXIFG));
    UCA0TXBUF = c;
}

void UART_init_1MHz(void){
    P1SEL  |= BIT1 + BIT2;
    P1SEL2 |= BIT1 + BIT2;
    UCA0CTL1 |= UCSWRST;
    UCA0CTL1 |= UCSSEL_2;

    // Configuração exata para 9600 bps @ 1 MHz
    UCA0BR0 = 104;     // <-- MUDOU DE 0xA0 (160) PARA 104
    UCA0BR1 = 0x00;    // <-- MUDOU DE 0x01 PARA 0x00
    UCA0MCTL = UCBRS_1; // Modulação recomendada para 9600

    UCA0CTL1 &= ~UCSWRST;
}

void UART_init_dinamico(unsigned int delta) {
    /*P1SEL  |= BIT1 + BIT2;
    P1SEL2 |= BIT1 + BIT2;

    UCA0CTL1 |= UCSWRST;
    UCA0CTL1 |= UCSSEL_2;*/

    UCA0BR0 = delta & 0xFF;
    UCA0BR1 = (delta >> 8) & 0xFF;

    UCA0MCTL = UCBRS_6;

    //UCA0CTL1 &= ~UCSWRST;
}

#pragma vector=TIMER1_A1_VECTOR
__interrupt void TIMER1_A1_ISR_HOOK(void) {
    switch (TA1IV) {
        case TA1IV_TACCR2:
            TA1CCR2 += TEMPO_TICK;
            tick++;
            break;
    }
}
