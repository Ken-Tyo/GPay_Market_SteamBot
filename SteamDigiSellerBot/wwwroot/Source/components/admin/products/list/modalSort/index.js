import React, { useState, useEffect, useRef } from 'react';
import sortItem from "../../../../../icons/sort.svg";

const ToggleSort = () => {
    const [isOpen, setIsOpen] = useState(false);
    const toggleRef = useRef(null);

    const handleSort = () => {
        setIsOpen(!isOpen);
    };

    const handleClickOutside = (event) => {
        if (toggleRef.current && !toggleRef.current.contains(event.target)) {
            setIsOpen(false);
        }
    };

    useEffect(() => {
        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, []);

    return (
        <div ref={toggleRef} style={{ position: 'relative', display: 'inline-block' }}>
            <div onClick={handleSort} style={{ cursor: 'pointer' }}>
                <img src={sortItem} style={{ marginLeft: "10px", cursor: "pointer" }} />
            </div>
            {isOpen && (
                <div style={{
                    position: 'absolute',
                    top: '100%',
                    left: '0',
                    backgroundColor: 'white',
                    border: '1px solid black',
                    padding: '10px',
                    zIndex: '1000'
                }}>
                    <p>Percents</p>
                    <p>Numbers</p>
                </div>
            )}
        </div>
    );
};

export default ToggleSort;