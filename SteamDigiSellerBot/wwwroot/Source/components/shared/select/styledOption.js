import React, { useState } from 'react';
import SelectUnstyled, {
  selectClasses as selectUnstyledClasses,
} from '@mui/base/Select';
import OptionUnstyled, {
  optionClasses as optionUnstyledClasses,
} from '@mui/base/Option';
import PopperUnstyled from '@mui/base/Popper';
import { styled } from '@mui/system';
import css from './styles.scss';

const blue = {
    100: '#DAECFF',
    200: '#99CCF3',
    400: '#3399FF',
    500: '#007FFF',
    600: '#0072E5',
    900: '#003A75',
  };
  
  const grey = {
    50: '#f6f8fa',
    100: '#eaeef2',
    200: '#d0d7de',
    300: '#afb8c1',
    400: '#8c959f',
    500: '#6e7781',
    600: '#57606a',
    700: '#424a53',
    800: '#32383f',
    900: '#24292f',
  };

const StyledOption = styled(OptionUnstyled)(
    ({ theme }) => `
    font-family: 'Igra Sans';
    list-style: none;
    padding: 8px;
    border-radius: 8px;
    cursor: pointer;
    color: #B3B3B3;
    font-size: 14px;
    line-height: 14px;
  
    &:last-of-type {
      border-bottom: none;
    }
  
    &.${optionUnstyledClasses.selected} {
      background-color: none;
      color: #FFFFFF;
    }
  
    &.${optionUnstyledClasses.highlighted} {
      background-color: none;
      color: #FFFFFF;
    }
  
    &.${optionUnstyledClasses.highlighted}.${optionUnstyledClasses.selected} {
      background-color: none;
      color: #FFFFFF;
    }
  
    &.${optionUnstyledClasses.disabled} {
      color: ${theme.palette.mode === 'dark' ? grey[700] : grey[400]};
    }
  
    &:hover:not(.${optionUnstyledClasses.disabled}) {
      background-color: none;
      color: #FFFFFF;
    }
    `
  );
  
  const StyledPopper = styled(PopperUnstyled)`
    z-index: 1400;
  `;

  export default StyledOption;