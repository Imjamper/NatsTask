::-s, --subject      NATS streaming server subject. Default - 'NATS_TASK'

::-c, --clusterId    NATS streaming server cluster name. Default = 'test-cluster'

::-u, --url          NATS streaming server url. Default = 'nats://localhost:4222'
.\NatsSubscriber\bin\Release\netcoreapp3.1\NatsSubscriber.exe --u "nats://192.168.99.100:4222" --s "subj1"