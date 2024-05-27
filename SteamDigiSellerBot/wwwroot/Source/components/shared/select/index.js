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

const CreateStyledButton = (width, color) =>
  styled('button')(
    ({ theme }) => `
  font-family: 'Igra Sans';
  font-size: 14px;
  line-height: 14px;
  box-sizing: border-box;
  width: ${width || 226}px;
  height: 51px;
  padding: 12px;
  border-radius: 15px;
  text-align: left;
  background: #512068;
  color: ${color || '#FFFFFF'};
  border: none;
  //z-index: 2;
  //position: relative;

  transition-property: all;
  transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  transition-duration: 120ms;

  &.${selectUnstyledClasses.focusVisible} {
    border-color: ${blue[400]};
    outline: 3px solid ${theme.palette.mode === 'dark' ? blue[500] : blue[200]};
  }

  &.${selectUnstyledClasses.expanded} {
    &::after {
      content: '▴';//url(../../../../../icons/pen.svg);
      // content: url("data:image/svg+xml,<svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
      // <path d="M15.9077 2.87207C15.8161 2.7408 15.6687 2.65967 15.5086 2.65283L5.58367 2.22502C5.2989 2.21256 5.06093 2.43241 5.04876 2.71585C5.03666 2.99918 5.25615 3.23858 5.53955 3.25075L14.7926 3.64963L12.9732 9.3261H4.87696L3.41425 1.36173C3.3821 1.18718 3.26226 1.04156 3.09697 0.976713L0.701303 0.0355518C0.437269 -0.0678201 0.139327 0.0618692 0.0356332 0.32558C-0.0678819 0.589435 0.0617716 0.887556 0.325662 0.99125L2.4558 1.82807L3.94432 9.93222C3.98919 10.1758 4.20152 10.3528 4.44933 10.3528H4.69625L4.13241 11.919C4.08522 12.0501 4.10466 12.1958 4.18498 12.3098C4.26518 12.4238 4.39562 12.4916 4.53487 12.4916H4.93035C4.68529 12.7644 4.53487 13.1235 4.53487 13.5184C4.53487 14.3676 5.22589 15.0585 6.07496 15.0585C6.92403 15.0585 7.61505 14.3676 7.61505 13.5184C7.61505 13.1235 7.46463 12.7644 7.21961 12.4916H10.5774C10.3322 12.7644 10.1818 13.1235 10.1818 13.5184C10.1818 14.3676 10.8727 15.0585 11.7219 15.0585C12.5712 15.0585 13.262 14.3676 13.262 13.5184C13.262 13.1235 13.1116 12.7644 12.8666 12.4916H13.3476C13.5839 12.4916 13.7754 12.3001 13.7754 12.0639C13.7754 11.8275 13.5839 11.6361 13.3476 11.6361H5.14357L5.60554 10.3527H13.3476C13.5708 10.3527 13.7683 10.2084 13.8363 9.99603L15.9754 3.32226C16.0245 3.16994 15.9993 3.0034 15.9077 2.87207ZM6.075 14.203C5.69749 14.203 5.39049 13.8961 5.39049 13.5186C5.39049 13.1411 5.69749 12.834 6.075 12.834C6.4525 12.834 6.75946 13.1411 6.75946 13.5186C6.75946 13.8961 6.4525 14.203 6.075 14.203ZM11.7219 14.203C11.3444 14.203 11.0375 13.8961 11.0375 13.5186C11.0375 13.1411 11.3444 12.834 11.7219 12.834C12.0994 12.834 12.4064 13.1411 12.4064 13.5186C12.4064 13.8961 12.0994 14.203 11.7219 14.203Z" fill="#B3B3B3"/>
      // </svg>");
      // width: 16px;
      // height: 16px;
      color: #FFFFFF;
    }
  }

  &::after {
    content: '▾';
    float: right;
    color: #FFFFFF;
  }
  `
  );

const CreateStyledListbox = (width, height) =>
  styled('ul')(
    ({ theme }) => `
  font-family: 'Igra Sans';
  font-size: 14px;
  line-height: 14px;
  box-sizing: border-box;
  padding: 6px 6px 6px 6px;
  margin-top: 10px;
  margin-left: 0px;
  //position: relative;
  //z-index: -100;
  width: ${width || 226}px;
  height: ${height || 155}px;
  border-radius: 15px;
  //border-radius: 0px 0px 15px 15px;
  overflow: auto;
  outline: 0px;
  background: #472159;
  border: none;
  color: ${theme.palette.mode === 'dark' ? grey[300] : grey[900]};
  box-shadow: none;

  &::-webkit-scrollbar {
    width: 14px
  }

  &::-webkit-scrollbar-thumb {
      border: 6px solid transparent;
      background-clip: padding-box;
      border-radius: 9999px;
      background-color: #83409b
  }
  `
  );

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

const defaultOption = (i) =>{
        <StyledOption
            key={i.name}
            value={i.name}
            style={{ color: i.color || '#B3B3B3' }}
          > 
          {i.name}
          </StyledOption>
}

export default function MultipleSelectPlaceholder({
  props,
  customRenderChild = null,
  options,
  multiple = false,
  defaultValue,
  onChange,
  hint,
  width,
  height,
}) {
  const [rootColor, setRootColor] = useState('#FFFFFF');
  const handleChange = (event, newValue) => {
    if (onChange) onChange(newValue);
    let currOpt = options?.find((o) => o.name === newValue);
    if (currOpt) setRootColor(currOpt.color);
  };

  const CustomSelect = React.forwardRef(function CustomSelect(props, ref) {
    const slots = {
      root: CreateStyledButton(width, rootColor),
      listbox: CreateStyledListbox(width, height),
      popper: StyledPopper,
      ...props.slots,
    };

    return <SelectUnstyled {...props} ref={ref} slots={slots} />;
  });
  const renderItem = customRenderChild == null?defaultOption:customRenderChild;
  return (
    <div className={css.wrapper} style={{ width: width }}>
      <CustomSelect {...props} defaultValue={defaultValue} onChange={handleChange} multiple={multiple}>
        
        {(options || []).map((i) => (
          renderItem(i)
        ))}

      </CustomSelect>
      {hint && <div className={css.hint}>{hint}</div>}
    </div>
  );
}
