<?PHP

    require 'wsmsclient.php';

    $iid = 1001;
    $productId = 102;
    $userId = 'developer';
    $groupId = 'developers';        //Optional  - can be an empty string
    $expHours = 1;
    $keyPath = 'private_key.pem';

    var_dump( WSMSClient::createSession($iid, $productId, $userId, $groupId, $expHours, $keyPath));

?>