import React from 'react';
import './Quote.css';

export interface QuoteData {
    id: number,
    content: string,
    admin: boolean
}

export class Quote extends React.Component<QuoteData, {}> {
    render() {
        const {id, content, admin} = this.props;
        var button = null;
        var contentName = "QuoteContent"
        if(admin){
            button = <button className="DeleteButton">Delete</button>
            contentName = "QuoteContent Admin"
        }
        return (
            <div className="Quote">
                <span className="QuoteIdBox"><span className="QuoteId">{id}</span></span>
                <span className={contentName}>{content}</span>
                {button}
            </div>
        );
    }
}