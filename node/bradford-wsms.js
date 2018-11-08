'use strict';

var njwt = require('njwt');


function verifyClaims(claimsObj) {

    var valid;

    var reqKeys = ['iss', 'userId', 'teamId', 'orderId', 'email', 'productId', 'jti'];

    var keys = Object.keys(claimsObj);

    valid = reqKeys.every(function(key){

        var ret = true;

        if (keys.indexOf(key) >= 0) {
            
            if (String(claimsObj[key]) === '')
                ret = false;
        }
        else
           ret = false;

        return ret;
    })

    return valid;

}

exports.createSession = function(kid, claims, expHours, pKey64, options, data) {

    //---------
        var pKey, jwt;
    //---------

    //Check all arguments ------
    if (arguments.length < 4)
        throw new Error('Required arguments are missing');


    if (typeof claims !== 'object')
        throw new Error('Invalid claims');

    if (!kid)
        throw new Error('Invalid integrator id');


    if (isNaN(expHours))
        throw new Error('No proper expiration set')
    else
        if (expHours > 24)
            throw new Error('Expiration out of range')
    
    if (!pKey64)
        throw new Error('Invalid private key')
    else {
        try {
            pKey = Buffer.from(pKey64, 'base64')                
        }
        catch(e) {
            throw new Error('Invalid private key encoding')
        }
    }

    if (data) {
        try {
            data = JSON.stringify(data);
        }
        catch(e) {
            throw new Error('Invalid data format')
        }
    }
    else
        data = '';

    
    //Handle options
    if (options)
        if (options.randomJti) {
            var uuid = require('uuid');
            Object.defineProperty(claims, 'jti', {value: uuid(), enumerable: true });
        }

    if (!verifyClaims(claims))
        throw new Error('Incomplete claims object');


    //Prepare the claims object ---------------
    if (!claims.iat)
        Object.defineProperty(claims, 'iat', { value: Math.floor(new Date().getTime() / 1000) });
    else
        claims.iat = Math.floor(new Date().getTime() / 1000);

    
    //Create the jwt -------------
    jwt =  njwt.create(claims, pKey, 'RS256')
        .setHeader('kid', kid)
        .setExpiration(Math.floor(new Date().getTime()) + 1000 * 60 * 60 * expHours);

    jwt = jwt.compact();

    //Return a prmise with a http request ------------
    return new Promise(function(resolve, reject){

        var http = require('https');
        var url = require('url');

        var urlOptions = url.parse('https://clickforms.appraisalworld.com/wsms/sessions');
    
        urlOptions.headers = {
            'authorization': 'Bearer ' + jwt,
            'Content-Type': 'application/json'
        }

        urlOptions.method = 'post';
                
        var getReq = http.request(urlOptions, (responseHandler) => {
            
            var buff;

            if (responseHandler.statusCode !== 201) {                             
                reject();
                return;
            }
    
           
            responseHandler.on('data', (chunk) => {
                buff += chunk;
            });
    
            responseHandler.on('end', () => {                
                resolve(); 
            });
    
        })
        .on('error', (e) => {            
            reject(e);
            return;
        });
    
       
           
        getReq.write(data);
        getReq.end();


    })
    
    
}