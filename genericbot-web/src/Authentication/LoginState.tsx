import React, { useState, useEffect } from 'react'
import './LoginState.css';
import Cookies from 'universal-cookie';
import { Link } from 'react-router-dom';

export default function LoginState(){
    const cookies = new Cookies();
    const [username, setUsername] = useState("");
    var loginMessage: any = undefined;
    useEffect(() => {
        if(cookies.get("Authorization")==undefined) {return;}
        const apiHeaders = new Headers();
        apiHeaders.set("Authorization", cookies.get("Authorization"))
        const userInfoREquest = new Request("/api/userinfo", {
            method: 'GET',
            headers: apiHeaders,
        });
        fetch(userInfoREquest)
        .then(result => {
            return result.text();    
        })
        .then(result => {
            let data = JSON.parse(result);
            setUsername(" as " + data.username);
        });
    })

    if(cookies.get("Authorization")==undefined) {
        loginMessage = (
            <Link to="/login">
                <p>not logged in</p>
            </Link>
            )
    } else {
        loginMessage = <p>logged in{username}</p>
    }

    return(
        <div className="LoginState">
        <hr/>
        {loginMessage}
        </div>
    )
}