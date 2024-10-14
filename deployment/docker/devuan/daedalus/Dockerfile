FROM devuan/devuan:daedalus

COPY gnomeshade.deb /gnomeshade.deb

RUN apt-get update && apt-get install -y libicu72 libssl3 libgssapi-krb5-2
RUN dpkg -i /gnomeshade.deb

EXPOSE 5000
