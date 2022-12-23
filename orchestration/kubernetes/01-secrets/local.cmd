@ECHO off

ECHO Creating secrets
ECHO.

kubectl create secret generic secrets-seelanstyres --namespace seelanstyres ^
    --from-literal=AdminCredentials__Email="%AdminCredentials__Email%" ^
    --from-literal=AdminCredentials__Password="%AdminCredentials__Password%" ^
    --from-literal=EmailCredentials__Email="%EmailCredentials__Email%" ^
    --from-literal=EmailCredentials__Password="%EmailCredentials__Password%"
