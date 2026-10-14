/*
 * Utilidades.h
 *
 *  Created on: 5 de mar de 2024
 *      Author: evert
 */

#ifndef UTILIDADES_H_
#define UTILIDADES_H_

//funções do sistema
void set_frequency(unsigned char freq_MHz, unsigned char* vetor_dco);
unsigned int get_adc_cal(unsigned int constante, unsigned int* vetor_adc);
void setup_tick(unsigned int tempo_tick);

#endif /* UTILIDADES_H_ */
