PROG=dash
CFLAGS= -Wall
CC=gcc
LD=gcc
LDFLAGS=

CAIROCFLAGS=`pkg-config --cflags cairo`
CAIROLDFLAGS=`pkg-config --libs cairo`

all: main


main: main.o
	$(LD) $(LDFLAGS) ${CAIROLDFLAGS} -o dash main.o

main.o: main.c
	$(CC) $(CFLAGS) ${CAIROCFLAGS} -c main.c

clean:
	rm -f $(PROG) *.o
