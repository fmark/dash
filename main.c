#define FRAMES_PER_SEC 25.0
#define DATAREADS_PER_SEC 1.0
#define SLEEPUSEC ((int)(1000000.0 / FRAMES_PER_SEC))
#define DATAREAD_FRAMESKIP ((int)(FRAMES_PER_SEC / DATAREADS_PER_SEC))

#define MSG_LEN 7

#include <stdio.h>   /* Standard input/output definitions */
#include <string.h>  /* String function definitions */
#include <unistd.h>  /* UNIX standard function definitions */
#include <fcntl.h>   /* File control definitions */
#include <errno.h>   /* Error number definitions */
#include <termios.h> /* POSIX terminal control definitions */

int init_alltrax(int ttyNum,  struct termios *oldtio){
  int fd; 
  //Path string functions
  const char *pathprefix = "/dev/ttyS";
  const int prefixln = strlen(pathprefix);
  char path[prefixln + 2 + 1];
  int charsln;
  //port settings
  struct termios newtio;

  // String manipulation to turn the ttyNum into a file path
  strncpy(path, pathprefix, prefixln);
  charsln = snprintf(path + prefixln, 3, "%d", ttyNum);
  path[prefixln + charsln] = '\0';

  //  printf("path: %s \n", path);

    
  fd = open(path, O_RDWR | O_NOCTTY);
  if (fd == -1){
      printf("path: %s \n", path);
      perror("open_port: Unable to open tty ");
    }

  tcgetattr(fd, oldtio); /* save current serial port settings */
  bzero(&newtio, sizeof(newtio)); /* clear struct for new port settings */
        
  /* 
          BAUDRATE: Set bps rate. You could also use cfsetispeed and cfsetospeed.
          CRTSCTS : output hardware flow control (only used if the cable has
                    all necessary lines. See sect. 7 of Serial-HOWTO)
          CS8     : 8n1 (8bit,no parity,1 stopbit)
          CLOCAL  : local connection, no modem contol
          CREAD   : enable receiving characters
  */
  //  newtio.c_cflag = B9600 | CRTSCTS | CS8 | CLOCAL | CREAD;
  newtio.c_cflag = B9600 | CS8 | CLOCAL | CREAD | IGNPAR;
         
  //      IGNPAR  : ignore bytes with parity errors
  //   newtio.c_iflag = IGNPAR;
  //   newtio.c_oflag = 0;
        
  /* set input mode (non-canonical, no echo,...) */
  newtio.c_lflag = 0;

         
  //  newtio.c_cc[VTIME]    = 10;   /* inter-character timer unused */
  newtio.c_cc[VMIN]     = MSG_LEN; 
        
  tcflush(fd, TCIFLUSH);
  tcsetattr(fd,TCSANOW,&newtio);
         


  return fd;
}

int main(int argc, char** argv){
  int fd;
  struct termios oldtio;
  unsigned char input[MSG_LEN], output[MSG_LEN];
  int res;

  //init, create surface, etc.
  fd = init_alltrax(0, &oldtio);

  int dataskip_counter = 0;
  do {
    
    //update stats every x frames
    if ((dataskip_counter++ % DATAREAD_FRAMESKIP) == 0){
      //update state
      printf("updating:\n\twriting query...\n");
      //marshall output message:
      output[0] = 0x5B;
      output[1] = 0x04;
      output[2] = 0x39;
      output[3] = 0;
      output[4] = 0;
      output[5] = 0;
      output[6] = output[0] + output[1] + output[2] + output[3] + output[4] + output[5];
      printf("\twrote:\n\t\t 0: %2x\n\t\t 1: %2x\n\t\t 2: %2x\n\t\t 3: %2x\n\t\t 4: %2x\n\t\t 5: %2x\n\t\t 6: %2x\n\n",
	     output[0], output[1], output[2], output[3], output[4], output[5], output[6]);

      if ((res = write(fd, output, MSG_LEN)) > MSG_LEN){
	printf("\twrite returned %d, not %d, quitting\n", res, MSG_LEN);
	break;
      } 
      printf("\twrote query.\n\treading...\n"); 
      if ((res = read(fd, input, MSG_LEN)) < MSG_LEN){
	printf("\tread returned %d, not %d, quitting\n", res, MSG_LEN);
        break;
      } 
      printf("\trecved:\n\t\t 0: %2x\n\t\t 1: %2x\n\t\t 2: %2x\n\t\t 3: %2x\n\t\t 4: %2x\n\t\t 5: %2x\n\t\t 6: %2x\n\n",
	     input[0], input[1], input[2], input[3], input[4], input[5], input[6]);
    }

    //draw 
    //    printf("draw\n");

    //instead of having a fixed interval here, we should
    //sleep for the fixed interval - the time it takes to
    //execute the loop body
    //  } while (usleep(SLEEPUSEC) == 0);
  } while (0);
 
  //cleanup 
  tcsetattr(fd, TCSANOW, &oldtio);
  close(fd);
  printf("Goodbye.\n");
 
  return 0;
}
