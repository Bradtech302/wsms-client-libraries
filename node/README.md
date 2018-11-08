# Introduction

The Bradford WSMS Client Library (Node JS) enables the integrator a convenient and faster way of integration.


# Installation

Execute the following NPM command –

    npm install bradford-wsms-client-library

Or, add the following line to the package.json under “dependancies”

    “bradford-wsms-client-library”:  “1.0.0”

Import the package into your applicastion

    const wsms = require(‘bradford-wsms-client-library’)

## Creating a Session

Use the `createSession` function to create a session for the cloud product you prefer.

## Arguments

|Argumen  | Type |
|--|--|
|  Integrator-id | String |
|  Claims | JS Object |
|  Private Key | String (Base 64) |
|  Options | JS Object (optional) |
|  Data | JS Object (optional) |


## Options

**randomJti**  :  

A random jti will automatically be created if true. The claims should include jti if false



## Code Sample

    var privateKey = fs.readFileSync('./private_key.pem');
    
    var claims = {    
       iss: 'sample-integrator',    
       userId: 'user-1',    
       teamId: 0,    
       orderId: 'order-1',    
       email: 'user-1@example.com',    
       productId: 101,    
    }
    
    var pKey = Buffer.from(privateKey).toString('base64');    
    var options = {randomJti: true};    
    var data = {hello: ‘world’}
    
    wsms.createSession('sample-id', claims, 3, pKey, options, data)    
    .then(function(){    
          console.log(‘Session Created’);
       },    
       function(e) {    
          console.log(e);
    })


## Security

The loading of the private key from a disk file in the above example is for demonstration purposes only. It is advisable to use environment variables to supply the key to the container running your app.