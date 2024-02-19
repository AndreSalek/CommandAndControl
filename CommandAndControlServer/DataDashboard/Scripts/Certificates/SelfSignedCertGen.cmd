if not [%1]==[] (
	set /p config=Enter openssl.cnf path:
) else set "config=%1"

set "OPENSSL_CONF=%config%"
openssl req -newkey rsa:2048 -nodes -keyout key.pem -x509 -days 365 -out certificate.pem
openssl x509 -text -noout -in certificate.pem 
openssl pkcs12 -inkey key.pem -in certificate.pem -export -out certificate.p12
openssl pkcs12 -in certificate.p12 -noout -info

del key.pem
del certificate.pem
