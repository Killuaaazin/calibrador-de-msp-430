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
void UART_sendNum(unsigned char num);
void Set_DCO(unsigned int Delta);

char DCOCTL_calibrado_1MHz, BCSCTL1_calibrado_1MHz, DCOCTL_nova_freq, BCSCTL1_nova_freq;
int main(void) {
	WDTCTL = WDTPW | WDTHOLD;

	P1DIR &= ~BIT5;
	P1REN &= ~(BIT5 + BIT2);
	P1DIR |= tp + tu;
	P1REN &= ~tp;
	P1OUT &= ~tp;
	P1REN |= tu;
	P1OUT |= tu;

	P1DIR |= BIT0 + BIT6;
	P1OUT &= ~(BIT0 + BIT6);

	// ========================================================
	// ETAPA 1: PRÉ-CALIBRAÇÃO DE SEGURANÇA
	// ========================================================
	P1OUT |= BIT6; // Liga LED Verde
	Set_DCO(416);//esse 416 é para 1 MHz
	DCOCTL_calibrado_1MHz = DCOCTL;//salvo nessas duas variáveis
	BCSCTL1_calibrado_1MHz = BCSCTL1;//o valor do DCO e do BCSCTL1
	//para reuso, caso usuario queira freq diferente de 1MHz
	P1OUT &= ~BIT6; // Apaga LED Verde

	// ========================================================
	// ETAPA 2: LEITURA SEGURA COM ANTI-LIXO
	// ========================================================
	UART_init_1MHz();

volta_inicio://
	UART_sendChar('R');

	P1OUT |= BIT0; // Liga LED Vermelho (À espera da palavra-passe)

	// Ignora os pulsos 0x55 que ficaram atrasados no cabo
	unsigned char sync;
	do {
		sync = UART_read_char();
	} while (sync != 0xAA);

	// Palavra-passe recebida! Lê os valores reais:
	//aqui seria obter a parte alta e baixa da nova frequencia em kHz
	//exemplo: 15500 [kHz] ou 15.5 MHz. O usuario digita 15500 e o C# envia a parte
	//alta desse valor e a parte baixa:
	//C# envia parte da alta deste 15500 = 15500/256 = 60;
	//C# envia parte da baixa deste 15500 = 15500%256 = 140;
	unsigned char byte_alto = UART_read_char();//byte_alto = 60
	unsigned char byte_baixo = UART_read_char();////byte_baixo = 140
	unsigned int target_delta = (byte_alto << 8) | byte_baixo;

	unsigned long target_delta_long;
	target_delta_long = ((unsigned long)target_delta * 	(unsigned long)(416))/
			(unsigned long)1000;

	target_delta = (unsigned int)target_delta_long;

	// ========================================================
	// ETAPA 3: CALIBRAÇÃO FINAL
	// ========================================================


	P1OUT |= BIT6; // Ambos acesos (Verde + Vermelho)
	/* * nao precisa disso
        // Seleciona a faixa de hardware adequada (RSEL) com base na frequência desejada
        if (target_delta > 5500) { // Se for em torno de 16 MHz (Delta aprox. 6666)
            BCSCTL1 = 0x0F;        // Força a faixa máxima absoluta de hardware (RSEL.3 = 1)
        }
        else if (target_delta > 3300) { // Se for acima de ~8 MHz (ex: 12 MHz)
            BCSCTL1 = 0x0E;
        }
        else if (target_delta > 1500) { // Se for em torno de 8 MHz (Delta aprox. 3333)
            BCSCTL1 = 0x0B;
        }
        // Se for 1 MHz, mantém o padrão da pré-calibração
	 */
	Set_DCO(target_delta);
	DCOCTL_nova_freq = DCOCTL;//nova frequencia
	BCSCTL1_nova_freq= BCSCTL1;
	P1OUT &= ~(BIT0 + BIT6); // Apaga ambos



	// ========================================================
	// ETAPA 4: ENVIO DOS DADOS PURAMENTE EM BYTES AO PC
	// ========================================================
	//nao precisa disto=> UART_init_dinamico(target_delta);

	// Envia o 'q' e os dois registradores diretamente como bytes puros

	DCOCTL = DCOCTL_calibrado_1MHz;//devolvo para 1 MHz para a comunicação
	BCSCTL1 = BCSCTL1_calibrado_1MHz;//voltar como era antes em 1 MHz

	UART_sendChar('q');
	UART_sendChar(DCOCTL_nova_freq);
	UART_sendChar(BCSCTL1_nova_freq);


	setup_tick(TEMPO_TICK);
	__enable_interrupt();
	Temporizador t_pisca;
	inicializa_temporizador(100, &t_pisca);

	IFG2 &= ~UCA0RXIFG;

	while(1) {
		if (passou_tempo(&t_pisca)) {
			reseta_temporizador(&t_pisca);
			P1OUT ^= BIT0 + BIT6;
		}

		if (IFG2 & UCA0RXIFG)
		if (UART_read_char() == 'N')//C# envia para o MSP fazer com novo valor
			goto volta_inicio;//para refazer a calibração com outra frequencia
	}//while

}//Main

// ----------------------------------------------------
// A função de calibração mantém a tolerância de segurança
// ----------------------------------------------------
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

				if (Compare >= Delta - 2 && Compare <= Delta + 2) {
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

void UART_sendNum(unsigned char num) {
	UART_sendChar((num / 100) + '0');
	UART_sendChar(((num / 10) % 10) + '0');
	UART_sendChar((num % 10) + '0');
}

void UART_init_1MHz(void){
	P1SEL  |= BIT1 + BIT2;
	P1SEL2 |= BIT1 + BIT2;
	UCA0CTL1 |= UCSWRST;
	UCA0CTL1 |= UCSSEL_2;
	UCA0BR0 = 0xA0;
	UCA0BR1 = 0x01;
	UCA0MCTL = UCBRS_6;
	UCA0CTL1 &= ~UCSWRST;
}

void UART_init_dinamico(unsigned int delta){
	P1SEL  |= BIT1 + BIT2;
	P1SEL2 |= BIT1 + BIT2;
	UCA0CTL1 |= UCSWRST;
	UCA0CTL1 |= UCSSEL_2;
	UCA0BR0 = delta & 0xFF;
	UCA0BR1 = (delta >> 8) & 0xFF;
	UCA0MCTL = UCBRS_6;
	UCA0CTL1 &= ~UCSWRST;
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
