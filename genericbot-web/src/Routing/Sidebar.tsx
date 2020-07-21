import React from 'react';
import './Sidebar.css';
import SidebarLink from './SidebarLink';
import LoginState from '../Authentication/LoginState'
import { Link } from 'react-router-dom';
 
function Sidebar() {
  return (
    <div className="Sidebar">
      <Link to="/">  
        <h1>GenericBot</h1>
        <h3>Web Dashboard</h3>
      </Link>
      <hr />
      <SidebarLink name="Quotes" url="/quotes"/>

      <LoginState/>
    </div>
  );
}

export default Sidebar;
