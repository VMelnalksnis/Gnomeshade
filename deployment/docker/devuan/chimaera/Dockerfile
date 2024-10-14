FROM devuan/devuan:chimaera

COPY gnomeshade.deb /gnomeshade.deb

RUN apt-get update && apt-get install -y libicu67 libssl1.1 libgssapi-krb5-2
RUN dpkg -i /gnomeshade.deb

EXPOSE 5000
