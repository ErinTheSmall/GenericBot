import React from 'react';
import './App.css';
import Sidebar from './Routing/Sidebar';
import Quotes from './Quotes/Quotes';
import {
  BrowserRouter as Router,
  Switch,
  Route,
} from 'react-router-dom'
import Login from './Authentication/Login';
import Callback from './Authentication/Callback';

function App() {
  return (
    <Router>
      <Sidebar/>
      <div className="App">
        <Switch>
          <Route path="/quotes/:paramId">
            <Quotes />
          </Route>
          <Route path="/quotes">
            <Quotes />
          </Route>
          <Route path="/login">
            <Login />
          </Route>
          <Route path="/callback">
            <Callback />
          </Route>
          <Route path="/">
            <h1 className="PageTitle">Home</h1>
          </Route>
        </Switch>
      </div>
    </Router>
  );
}

export default App;
