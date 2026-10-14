/*
 * Temporizador.h
 *
 *  Created on: 22/11/2023
 *      Author: evert
 */

#ifndef TEMPORIZADOR_H_
#define TEMPORIZADOR_H_
// Temporizador.h
struct temporizador {//definição do meu novo tipo de dado temporizador
	unsigned int ti;
	unsigned int tempo;//tempo desejado com base no tempo do tick
	char to;
};// fim da definição da estrutura
typedef struct temporizador Temporizador;
void inicializa_temporizador(unsigned int tempo, Temporizador* t);
char passou_tempo(Temporizador* t);
void reseta_temporizador(Temporizador* t);
#endif /* TEMPORIZADOR_H_ */
