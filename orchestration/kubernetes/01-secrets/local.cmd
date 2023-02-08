@ECHO off

ECHO Creating secrets
ECHO.

kubectl create secret generic secrets-global ^
    --from-literal=AzureAppConfig__Enabled="%AzureAppConfig__Enabled%" ^
    --from-literal=AzureAppConfig__ConnectionString="%AzureAppConfig__ConnectionString%" ^
    --from-literal=AppInsights__Enabled="%AppInsights__Enabled%" ^
    --from-literal=AppInsights__ConnectionString="%AppInsights__ConnectionString%" ^
    --from-literal=AdminCredentials__Email="%AdminCredentials__Email%" ^
    --from-literal=AdminCredentials__Password="%AdminCredentials__Password%" ^
    --from-literal=EmailCredentials__Email="%EmailCredentials__Email%" ^
    --from-literal=EmailCredentials__Password="%EmailCredentials__Password%"

kubectl create secret tls tls-for-health-local ^
    --cert "%~dp0health-local.seelanstyres.com.crt" ^
    --key  "%~dp0health-local.seelanstyres.com.key"

kubectl create secret tls tls-for-id-local ^
    --cert "%~dp0id-local.seelanstyres.com.crt" ^
    --key  "%~dp0id-local.seelanstyres.com.key"

kubectl create secret tls tls-for-www-local ^
    --cert "%~dp0www-local.seelanstyres.com.crt" ^
    --key  "%~dp0www-local.seelanstyres.com.key"

kubectl create secret tls tls-for-kibana-local ^
    --cert "%~dp0kibana-local.seelanstyres.com.crt" ^
    --key  "%~dp0kibana-local.seelanstyres.com.key"

kubectl create secret tls tls-for-rabbitmq-local ^
    --cert "%~dp0rabbitmq-local.seelanstyres.com.crt" ^
    --key  "%~dp0rabbitmq-local.seelanstyres.com.key"
