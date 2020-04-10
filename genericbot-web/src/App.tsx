import React from 'react';
import './App.css';
import Sidebar from './Routing/Sidebar';
import Quotes from './Quotes/Quotes';
import {
  BrowserRouter as Router,
  Switch,
  Route,
} from 'react-router-dom'

function App() {
  return (
    <Router>
      <Sidebar/>
      <div className="App">
      <Switch>
        <Route path="/quotes">
          <Quotes />
        </Route>
        <Route path="/">
          <h1>Home</h1>
        </Route>
      </Switch>
      </div>
    </Router>
  );
}

export default App;
