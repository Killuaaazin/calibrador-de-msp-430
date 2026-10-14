#include <msp430g2553.h>
#include "Ext.h"
// --- Hardware Mapping ---
#define   LED1   (1<<0)
#define   LED2   (1<<6)
#define   SENS   (1<<7)

// ============================================================================
// --- Functions Prototypes ---
void object_counter();
//void uart_init();
void uart_write_char(char c);

// ============================================================================
// --- Global Variables ---
unsigned counter = 0;
char flag_count = 0;
//extern volatile unsigned int vel;
// ============================================================================
// --- Functions ---
// ============================================================================
// --- object_counter ---
void object_counter() {
	static char flags = 0x00;

	if (!(P1IN & SENS))
		flags |= (1 << 0);

	if ((P1IN & SENS) && (flags&(1<<0))){//borda de subida
		flags &= ~(1<<0); counter++;
	}	//end if P1IN
	uart_write_char((char) (counter / 1000) + '0');
	uart_write_char((char) (counter % 1000 / 100) + '0');
	uart_write_char((char) (counter % 100 / 10) + '0');
	uart_write_char((char) (counter % 10) + '0');
	uart_write_char (    (char)( (vel %1000)/100 ) + '0');
	uart_write_char((char) (   (vel % 100) / 10) + '0');
	uart_write_char((char) (vel % 10) + '0');
	uart_write_char(0x0D);
	uart_write_char(0x0A);
} //end object_counter

// --- uart_write_char ---
void uart_write_char(char c){
  while(!(IFG2 & UCA0TXIFG));                  //aguarda o buffer de transmissão estar pronto
  UCA0TXBUF = c;                               //envia o caractere
} //end uart_write_char
