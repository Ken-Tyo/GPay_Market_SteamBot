import React from 'react';
import Select from '../../select'; 
import css from '../styles.scss';


const FormItemSelect = ({ name, onChange, value, options, hint }) => {
    return (
      <div className={css.formItem}>
        <div className={css.name}>{name}</div>
        <div>
          <Select
            options={options}
            defaultValue={value}
            onChange={onChange}
            hint={hint}
          />
        </div>
      </div>
    );
  };

  export default FormItemSelect;