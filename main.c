#define FRAMES_PER_SEC 25.0
#define DATAREADS_PER_SEC 1.0
#define SLEEPUSEC ((int)(1000000.0 / FRAMES_PER_SEC))
#define DATAREAD_FRAMESKIP ((int)(FRAMES_PER_SEC / DATAREADS_PER_SEC))

#include <stdio.h>
#include <unistd.h>

int main(int argc, char** argv){

  //init, create surface, etc.

  int dataskip_counter = 0;
  do {
    
    //update stats every x frames
    if ((dataskip_counter++ % DATAREAD_FRAMESKIP) == 0){
      //update state
      printf("update\n");
    }

    //draw 
    printf("draw\n");

    //instead of having a fixed interval here, we should
    //sleep for the fixed interval - the time it takes to
    //execute the loop body
  } while (usleep(SLEEPUSEC) == 0);

  //cleanup

  return 0;
}
