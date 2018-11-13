<?php


require __DIR__ . '/vendor/autoload.php';

use  Firebase\JWT\JWT;  //Namespace set by php-jwt




class WSMSClient {

    const  API_URL = 'https://clickforms.appraisalworld.com/wsms/sessions';

    public static function createSession($kid, $productId, $userId, $groupId, $expHours, $keyPath) {

        //Read the PEM file
        $privateKey =  file_get_contents($keyPath, true);


        //Assemble Claims
        $token = array(
            "iss" => "develop",
            "iat" => time(),
            "exp" => time() + ($expHours * 60 * 60),
            "jti" => self::guidv4(random_bytes(16)),                         
            "productId" => $productId,
            "userId" => $userId  
        );

        if (!$groupId !== "")
            $token["groupId"] = $groupId;
        

        //Create the token and sign
        $jwt = JWT::encode($token, $privateKey, 'RS256', $kid);
       

        // Prepare the request
        $options = array(
            'http' => array(
                'header'  => "Content-type: application/json\r\nauthorization: bearer " . $jwt,
                'method'  => 'POST',
            )
        );

        $context  = stream_context_create($options);

        //Make the request
        try {
            $result = file_get_contents(self::API_URL, false, $context);
        }
        catch(Exception $e) {
            throw new Exception("Error making the remote request");
        }
    
        //Handle the results
        if ($result === FALSE){ 
            throw new Exception("Failed creating the session");
        }
        else {
            return json_decode($result);
        }
    }


    private static function guidv4($data)
    {
        assert(strlen($data) == 16);
    
        $data[6] = chr(ord($data[6]) & 0x0f | 0x40); // set version to 0100
        $data[8] = chr(ord($data[8]) & 0x3f | 0x80); // set bits 6-7 to 10
    
        return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
    }
}



 
?>

