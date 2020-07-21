import React, { useState, useEffect } from 'react'      
import './Quotes.css';
import { QuoteData, Quote } from './Quote';
import { useParams } from 'react-router-dom';


function Quotes() {
    const [quotes, setQuotes] = useState<Array<QuoteData>>([]);
    const [isLoading, setIsLoading] = useState(true);
    const {paramId} = useParams();

    useEffect(() => {
        if(paramId===undefined) {return;}
        fetch("https://genericbot.galenguyer.com/api/quotes")
        .then(result => {
            return result.text();    
        })
        .then(result => {
            let newQuotes: QuoteData[] = JSON.parse(result);
            setQuotes(newQuotes);
            setIsLoading(false);
        });
    })

    return (
        <div className="Quotes">
        {paramId===undefined && <h3>Gotta have a guildid bucko</h3>
        }
        {quotes.map((quote) => (<Quote id={quote.id} content={quote.content} admin={false}/>))}
        </div>
    )

}

export default Quotes;