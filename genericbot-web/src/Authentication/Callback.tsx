import React, { useState, useEffect } from 'react'      
import queryString from 'query-string';
import Cookies from 'universal-cookie';

function Callback() {
    var code = queryString.parse(window.location.search).code+"";
    
    useEffect(() => {
        if(code==""||code=="yolo") {return;}
        fetch("/api/callback?code="+code)
        .then(result => {
            return result.text();    
        })
        .then(result => {
            console.log(result);
            code=result;
            var cookies = new Cookies();
            var expiryDate = new Date();
            expiryDate.setTime(expiryDate.getTime() + (7*24*60*60*1000));
            cookies.set("Authorization", result, {path: "/", expires: expiryDate});
            setTimeout(function () {
                window.location.pathname = "/"
            }, 3000);
        });
    })

    return (
        <div className="Quotes">
        <h1 className="PageTitle">Authorization Callback</h1>
        {code=="" && <h3>Gotta have a code bucko</h3>}
        {code!="" && (
            <p>Please hold tight! We're saving wrapping up getting you logged in. You'll be redirected back to the main page in a couple seconds</p>
        )}
        </div>
    )

}

export default Callback;