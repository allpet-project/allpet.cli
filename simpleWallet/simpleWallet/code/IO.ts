namespace tool {

    export function loadJson(url: string, callback: (json) => void) {
        var req = new XMLHttpRequest();
        req.open("GET", url);
        req.responseType="json"
        req.onreadystatechange = () => {
            if (req.readyState === 4) {
                callback(req.response);
            }
        }
        req.send();
    }
}