docker pull nats-streaming
docker run -d -p 4222:4222 -p 8222:8222 nats-streaming -store file -dir datastore