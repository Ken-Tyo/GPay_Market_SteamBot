import * as React from 'react';
import css from './styles.scss';

const SwitchBtn = ({ value, onChange, style }) => {
  const [checked, setChecked] = React.useState(false);
  React.useEffect(() => {
    setChecked(value);
  }, [value]);

  return (
    <div className={css.wrapper} style={{ ...style }}>
      <div
        className={css.track + ' ' + (checked ? css.checked : '')}
        onClick={() => {
          if (onChange) onChange(!checked);

          setChecked(!checked);
        }}
      >
        <div className={css.thumb + ' ' + (checked ? css.checked : '')}></div>
      </div>
    </div>
  );
};

export default SwitchBtn;
