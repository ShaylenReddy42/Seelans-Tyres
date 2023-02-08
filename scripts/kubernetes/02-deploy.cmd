@ECHO off

CD "%~dp0"

CD ..\..\orchestration\kubernetes

CALL 01-secrets\local.cmd

ECHO.
ECHO Creating global configmap
ECHO.

kubectl apply -f 02-configmaps\local.yaml

ECHO.
ECHO Creating local services
ECHO.

kubectl apply -f 03-local-services\

ECHO.
ECHO Creating deployments
ECHO.

kubectl apply -f 04-deployments\

ECHO.
ECHO Creating ingress resources
ECHO.

kubectl apply -f 05-ingress\local.yaml
kubectl apply -f 05-ingress\local-services.yaml

ECHO.
ECHO Run "03-delete.cmd" to cleanup the resources by deleting the namespace
ECHO.

PAUSE
