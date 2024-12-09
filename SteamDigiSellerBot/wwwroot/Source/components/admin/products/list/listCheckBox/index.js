import React, { useState } from 'react';
import css from '../../../../shared/checkBox/styles.scss'

const listCheckBox = ({ value, onCheckedChange }) => {
  const [checked, setChecked] = useState(false);
  React.useEffect(() => {
      setChecked(value);
   }, [value]);

  return (
    <div
      className={css.wrapper}
        onClick={(event) => {
        let newVal = !checked;
        setChecked(newVal);
        if (onCheckedChange) onCheckedChange(newVal, event.shiftKey);
      }}
    >
      {checked && <div className={css.checked}></div>}
    </div>
  );
};

export default listCheckBox;
