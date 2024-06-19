import * as React from "react";
import SelectUnstyled, {
  selectClasses as selectUnstyledClasses,
} from "@mui/base/Select";
import OptionUnstyled, {
  optionClasses as optionUnstyledClasses,
} from "@mui/base/Option";
import PopperUnstyled from "@mui/base/Popper";
import { styled } from "@mui/system";
import css from "./styles.scss";

const blue = {
  100: "#DAECFF",
  200: "#99CCF3",
  400: "#3399FF",
  500: "#007FFF",
  600: "#0072E5",
  900: "#003A75",
};

const grey = {
  50: "#f6f8fa",
  100: "#eaeef2",
  200: "#d0d7de",
  300: "#afb8c1",
  400: "#8c959f",
  500: "#6e7781",
  600: "#57606a",
  700: "#424a53",
  800: "#32383f",
  900: "#24292f",
};

const CreateStyledButton = (width) =>
  styled("button")(
    ({ theme }) => `
  font-family: 'Igra Sans';
  font-size: 14px;
  line-height: 14px;
  box-sizing: border-box;
  width: ${width || 226}px;
  height: 51px;
  padding: 0 0 0 15px;
  border-radius: 15px;
  text-align: left;
  background: #512068;
  color: #FFFFFF;
  border: none;

  display: flex;
  align-items: center;
  justify-content: space-between;

  //z-index: 2;
  //position: relative;

  transition-property: all;
  transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  transition-duration: 120ms;

  &.${selectUnstyledClasses.focusVisible} {
    border-color: ${blue[400]};
    outline: 3px solid ${theme.palette.mode === "dark" ? blue[500] : blue[200]};
  }

  &.${selectUnstyledClasses.expanded} {
    &::after {
      //content: '▴';//url(../../../../../icons/pen.svg);
      content: url("data:image/svg+xml,<svg class='MuiSvgIcon-root MuiSvgIcon-fontSizeMedium MuiSelect-icon MuiSelect-iconStandard css-1utq5rl' focusable='false' aria-hidden='true' viewBox='0 0 24 24' data-testid='ArrowDropDownIcon'><path d='M7 10l5 5 5-5z'></path></svg>");
      width: 24px;
      height: 24px;
      margin: 0 9px 0 0;
      transform: rotate(180deg);
    }
  }

  &::after {
    content: ' ';
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' class='MuiSvgIcon-root MuiSvgIcon-fontSizeMedium MuiSelect-icon MuiSelect-iconStandard' aria-hidden='true' viewBox='0 0 24 24' data-testid='ArrowDropDownIcon'%3E%3Cpath fill='%23FFFFFF' d='m7 10 5 5 5-5z'/%3E%3C/svg%3E");    //content: '▾';
    //content: url('data:image/svg+xml,<svg class="MuiSvgIcon-root MuiSvgIcon-fontSizeMedium MuiSelect-icon MuiSelect-iconStandard" aria-hidden="true" viewBox="0 0 24 24" data-testid="ArrowDropDownIcon"><path d="m7 10 5 5 5-5z"/></svg>');
    
    width: 24px;
    height: 24px;
    float: right;
    margin: 0 9px 0 0;
  }
  `
  );

const CreateStyledListbox = (width, height) =>
  styled("ul")(
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
  color: ${theme.palette.mode === "dark" ? grey[300] : grey[900]};
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
    color: ${theme.palette.mode === "dark" ? grey[700] : grey[400]};
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

export default function MultipleSelectPlaceholder({
  options,
  multiple,
  defaultValue,
  onChange,
  hint,
  width,
  height,
}) {
  const handleChange = (event, newValue) => {
    if (onChange) onChange(newValue);
  };

  const CustomSelect = React.forwardRef(function CustomSelect(props, ref) {
    const slots = {
      root: CreateStyledButton(width),
      listbox: CreateStyledListbox(width, height),
      popper: StyledPopper,
      ...props.slots,
    };

    return <SelectUnstyled {...props} ref={ref} slots={slots} />;
  });

  return (
    <div className={css.wrapper} style={{ width: width }}>
      <CustomSelect
        //renderValue={(e) => <span>{e}</span>}
        renderValue={(selected) => {
          return <span>{selected?.value}</span>;
        }}
        defaultValue={defaultValue}
        onChange={handleChange}
      >
        {(options || []).map((i) => (
          <StyledOption key={i.name} value={i.name}>
            {i.name}
          </StyledOption>
        ))}
      </CustomSelect>
      {hint && <div className={css.hint}>{hint}</div>}
    </div>
  );
}
