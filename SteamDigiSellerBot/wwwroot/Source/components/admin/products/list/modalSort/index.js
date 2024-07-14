import React, { useState, useEffect, useRef } from 'react';
import sortItem from "../../../../../icons/sort.svg";
import css from './styles.scss';

const ToggleSort = ({parentRef, onSort}) => {
    const [isOpen, setIsOpen] = useState(false);
    const toggleRef = useRef(null);

    const togglesortPriceModal = () => {
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
            <div onClick={togglesortPriceModal} style={{ cursor: 'pointer' }}>
                <img src={sortItem} style={{ marginLeft: "10px", cursor: "pointer" }} />
            </div>
            {isOpen && (
                <div className={css.sortPriceDropDown}>
                    <p className={css.sortPriceDropDownItem} onClick={() => onSort('percent')}>По процентам</p>
                    <p className={css.sortPriceDropDownItem} onClick={() => onSort('price')}>По сумме чисел</p>
                </div>
            )}
        </div>
    );
};

export default ToggleSort;