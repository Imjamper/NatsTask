::-s, --subject      NATS streaming server subject. Default - 'NATS_TASK'

::-c, --clusterId    NATS streaming server cluster name. Default = 'test-cluster'

::-u, --url          NATS streaming server url. Default = 'nats://localhost:4222'
.\NatsPublisher\bin\Debug\netcoreapp3.1\NatsPublisher.exe --u "nats://192.168.99.100:4222" --s "some_subject"