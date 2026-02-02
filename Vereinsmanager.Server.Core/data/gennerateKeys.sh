#!/bin/sh

# Create keys directory if it doesn't exist
mkdir -p ./keys

# Generate 4096-bit RSA private key (PKCS#1 PEM)
openssl genrsa -out ./keys/private_key.pem 4096

# Extract public key (PKCS#1 PEM)
openssl rsa -in ./keys/private_key.pem -pubout -out ./keys/public_key.pem

echo "RSA keypair generated in ./keys/"
