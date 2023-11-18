import React from 'react';
import css from './styles.scss';

const TextSwitch = ({ onChange, options, defaultValue }) => {
  const [checked, setChecked] = React.useState(false);
  React.useEffect(() => {
    setChecked(defaultValue);
  }, [defaultValue]);

  return (
    <div className={css.wrapper}>
      <div
        className={css.track}
        onClick={() => {
          if (onChange) onChange(!checked);

          setChecked(!checked);
        }}
      >
        <div className={css.options}>
          <div>{options[0]}</div>
          <div>{options[1]}</div>
        </div>
        <div className={css.thumb + ' ' + (checked ? css.checked : '')}></div>
      </div>
    </div>
  );
};

export default TextSwitch;
