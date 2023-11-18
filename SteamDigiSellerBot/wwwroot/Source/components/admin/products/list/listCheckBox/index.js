import React, { useState } from 'react';
import css from './styles.scss';

const listCheckBox = ({ value, onClick }) => {
  const [checked, setChecked] = useState(false);
  React.useEffect(() => {
    setChecked(value);
  }, [value]);

  return (
    <div
      className={css.wrapper}
      onClick={() => {
        let newVal = !checked;
        setChecked(newVal);
        if (onClick) onClick(newVal);
      }}
    >
      {checked && <div className={css.checked}></div>}
    </div>
  );
};

export default listCheckBox;
