
self.addEventListener('message', function (e) {

    //console.log('helpme');
    //var respond = get('http://localhost:8812/ClaimsPage/SubmitClaimsForm2', e.data.value);
    //console.log('Claims response' + respond);

 
    postMessage({ data: e.data.key ,resp: respond});
   
   

}, false);




function get(url,data) {
    try {
        var xhr = new XMLHttpRequest();
        var params = data;
        xhr.open('POST', url, true);
        xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
        xhr.send(params);
        return xhr.responseText;
    } catch (e) {
        return '0'; // turn all errors into empty results
    }
}