import React from 'react';
import './Login.css';
import queryString from 'query-string';

function Login() {
    return (
        <div className="Login">
            <h1 className="PageTitle">Login</h1>
            <p>Click here to log in with <a href={"https://discordapp.com/api/oauth2/authorize?client_id=295329346590343168&redirect_uri=" + encodeURI(window.location.origin) + "/callback&response_type=code&scope=guilds%20identify&prompt=none"}>Discord!</a></p>
            <h3>What information are you giving us?</h3>
            <p>I'll tell you later (rip styles)</p>
        </div>
    )
}
export default Login;