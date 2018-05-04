FROM debian:jessie

RUN apt-get update && apt-get install -y bc git build-essential
RUN apt-get install -y linux-headers-amd64
RUN git clone https://github.com/luigirizzo/netmap.git /usr/src/netmap
RUN cd /usr/src/netmap/LINUX && ./configure --kernel-dir=$(ls -d /usr/src/linux-headers-*-amd64) 
RUN cd /usr/src/netmap/LINUX && make
RUN cd /usr/src/netmap/LINUX && make apps
RUN cd /usr/src/netmap/LINUX && make install

RUN modprobe netmap

CMD .src/netmap/LINUX/build-apps/pkt-gen/pkt-gen -i eth0 -f tx -l 60
#RUN apt-get -y install kernel-package
#RUN apt-get -y install linux-source

#RUN git clone https://github.com/luigirizzo/netmap.git
#WORKDIR netmap/LINUX

#RUN ./configure
#RUN make
#RUN make apps
#RUN make install