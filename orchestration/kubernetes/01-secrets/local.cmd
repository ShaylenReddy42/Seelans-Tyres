@ECHO off

ECHO Creating secrets
ECHO.

kubectl create secret generic secrets-global --namespace seelanstyres ^
    --from-literal=AdminCredentials__Email="%AdminCredentials__Email%" ^
    --from-literal=AdminCredentials__Password="%AdminCredentials__Password%" ^
    --from-literal=EmailCredentials__Email="%EmailCredentials__Email%" ^
    --from-literal=EmailCredentials__Password="%EmailCredentials__Password%"

kubectl create secret tls tls-for-health-local --namespace seelanstyres ^
    --cert "%~dp0health-local.seelanstyres.com.crt" ^
    --key  "%~dp0health-local.seelanstyres.com.key"

kubectl create secret tls tls-for-id-local --namespace seelanstyres ^
    --cert "%~dp0id-local.seelanstyres.com.crt" ^
    --key  "%~dp0id-local.seelanstyres.com.key"

kubectl create secret tls tls-for-www-local --namespace seelanstyres ^
    --cert "%~dp0www-local.seelanstyres.com.crt" ^
    --key  "%~dp0www-local.seelanstyres.com.key"

kubectl create secret tls tls-for-kibana-local --namespace seelanstyres ^
    --cert "%~dp0kibana-local.seelanstyres.com.crt" ^
    --key  "%~dp0kibana-local.seelanstyres.com.key"

kubectl create secret tls tls-for-rabbitmq-local --namespace seelanstyres ^
    --cert "%~dp0rabbitmq-local.seelanstyres.com.crt" ^
    --key  "%~dp0rabbitmq-local.seelanstyres.com.key"
