import React from 'react';
import { Link } from "react-router-dom";
import './SidebarLink.css';

interface SidebarLinkProps {
    name: string,
    url: string,
}

class SidebarLink extends React.Component<SidebarLinkProps, {}> {
    render() {
        const {name, url} = this.props;
        return (
            <Link to={url}>
                <div className="SidebarLink">
                    {name}
                </div>
            </Link>
        );
    }
}

export default SidebarLink;