@ECHO off

ECHO Deleting all resources with the label "solution=seelanstyres"
ECHO.

kubectl delete configmaps,deployments,services,ingress -l solution=seelanstyres

ECHO.
ECHO Deleting secrets
ECHO.

kubectl delete secret secrets-global
kubectl delete secret tls-for-health-local
kubectl delete secret tls-for-id-local
kubectl delete secret tls-for-www-local
kubectl delete secret tls-for-kibana-local
kubectl delete secret tls-for-rabbitmq-local

ECHO.

PAUSE
