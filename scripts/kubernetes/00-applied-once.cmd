@ECHO off

ECHO Adding the official nginx ingress to the cluster
ECHO.

kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/cloud/deploy.yaml

ECHO.
ECHO A script to remove this will not be provided but if you want to clean this up,
ECHO 'kubectl delete namespace ingress-nginx' can be used to do so
ECHO The reason? Because I believe it should be apart of any cluster
ECHO.

PAUSE
