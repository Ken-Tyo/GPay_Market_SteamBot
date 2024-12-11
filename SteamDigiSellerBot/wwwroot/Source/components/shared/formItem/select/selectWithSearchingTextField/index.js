import React, { useState, useRef } from "react";
import {
  TextField,
  Popper,
  Paper,
  MenuItem,
  Checkbox,
  ListItemText,
  FormControl,
  ClickAwayListener
} from "@mui/material";
import { styled } from "@mui/system";
import { selectClasses as selectUnstyledClasses } from "@mui/base/Select";
import { optionClasses as optionUnstyledClasses } from "@mui/base/Option";
import PopperUnstyled from "@mui/base/Popper";

import css from "./styles.scss";
import checkBoxCss from "../../../checkBox/styles.scss";

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

const CreateStyledButton = (width, color) =>
  styled("button")(
    ({ theme }) => `
    font-family: 'Igra Sans';
    font-size: 14px;
    line-height: 14px;
    box-sizing: border-box;
    width: 226;
    height: 51px;
    border-radius: 15px;
    text-align: left;
    background: #512068;
    color: ${color || "#FFFFFF"};
    border: none;
    transition-property: all;
    transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
    transition-duration: 120ms;
    &.${selectUnstyledClasses.focusVisible} {
      border-color: ${blue[400]};
      outline: 3px solid ${theme.palette.mode === "dark" ? blue[500] : blue[200]};
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
    width: ${width || 226}px;
    height: ${height || 155}px;
    border-radius: 15px;
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

const bara = (width, height) => {
  return {
    fontFamily: "Igra Sans",
    fontSize: "14px",
    lineHeight: "14px",
    boxSizing: "border-box",
    padding: "6px",
    marginTop: "10px",
    marginLeft: "0px",
    width: `226px`,
    maxHeight: `155px`,
    borderRadius: "15px",
    overflow: "auto",
    outline: "0px",
    background: "#472159",
    border: "none",
    color: `${grey[900]}`,
    boxShadow: "none",
    "&::-webkit-scrollbar": {
      width: "14px",
    },
    "&::-webkit-scrollbar-thumb": {
      border: "6px solid transparent",
      backgroundClip: "padding-box",
      borderRadius: "9999px",
      backgroundColor: "#83409b",
    },
    fontFamily: "Igra Sans",
    cursor: "pointer",
    color: "#B3B3B3",
    fontSize: "14px",
    lineHeight: "14px",
    "&:last-of-type": {
      borderBottom: "none",
    },

    [`&.${optionUnstyledClasses.selected}`]: {
      backgroundColor: "none",
      color: "#FFFFFF",
    },

    [`&.${optionUnstyledClasses.highlighted}`]: {
      backgroundcolor: "none",
      color: "#FFFFFF",
    },

    [`&.${optionUnstyledClasses.highlighted}.${optionUnstyledClasses.selected}`]:
    {
      backgroundColor: "none",
      color: "#FFFFFF",
    },

    [`&.${optionUnstyledClasses.disabled}`]: {
      color: grey[400],
    },

    [`&:hover:not(.${optionUnstyledClasses.disabled})`]: {
      backgroundColor: "none",
      color: "#FFFFFF",
    },
  };
};

const StyledPopper = styled(PopperUnstyled)`
  z-index: 1400;
`;
const slots = {
  root: CreateStyledButton(undefined, "#FFFFFF"),
  listbox: CreateStyledListbox(undefined, undefined),
  popper: StyledPopper,
};

const MenuProps = {
  MenuListProps: { sx: { padding: "0px 0px" } },
  PaperProps: {
    sx: bara(undefined, undefined),
    style: {
      maxHeight: 155,
    },
  },
};

const SelectWithSearchingTextField = ({ options, onChange, value, placeholder }) => {
  const textFieldRef = useRef(null);
  const searchFieldRef = useRef(null);
  // const menuItemRef = useRef(null);

  const [anchorEl, setAnchorEl] = useState(null);
  const [isOpen, setIsOpen] = useState(false);
  const [search, setSearch] = useState("");

  const filteredOptions = options?.filter((option) => {
    if(!search)
      return true;
    return option.name?.toLowerCase().includes(search.toLowerCase());
  });

  const handleOpen = (e) => {
    setAnchorEl(textFieldRef.current);
    setIsOpen((prev) => !prev);
    if (textFieldRef.current) {
      textFieldRef.current.focus();
    }
  };

  const handleClose = (e) => {
    if (textFieldRef.current && textFieldRef.current.contains(e.target)) {
      return;
    }
    if (textFieldRef.current) {
      textFieldRef.current.blur();
    }

    setIsOpen(false);
  };

  const handleSearchChange = (e) => {
    setSearch(e.target.value);
  };

  const handleSelect = (item) => {
    const isSelected = value.some((currentItem) => currentItem.id === item.id);
    const newValue = isSelected
      ? value.filter((i) => i.id !== item.id)
      : [...value, item];
    if (!isSelected && searchFieldRef.current) {
      searchFieldRef.current.focus();
    }

    onChange({ target: { value: newValue } });

    return isSelected;
  };

  return (
    <FormControl sx={{ m: 1, width: "100%", height: "100% !important", margin: "0px" }}>
      <TextField
        variant="standard"
        sx={{
          height: "100% !important",
          cursor: "pointer",
          "& .MuiTextField-select.MuiInputBase-input ": {
            minHeight: "1em",
            maxWidth: "90%"
          },
          "& .MuiInputBase-input": {
            cursor: "pointer",
          },
          "& .MuiTextField-root": {
            height: "100% !important"
          },
          "& .MuiTextField-select.MuiInputBase-input span": {
            marginLeft: "12px",
          },
          "& .MuiInputBase-root.MuiInput-root ": {
            paddingRight: "9px",
            height: "100% !important",
            cursor: "pointer"
          },
          ".MuiTextField-nativeInput": {
            height: "0px",
            minHeight: "0px",
            padding: "0px !important",
            margin: "0px !important",
            border: "0px !important",
          },
          "& .MuiSvgIcon-root": {
            color: "rgb(255, 255, 255)",
            margin: "0 9px 0 0",
          },
        }}
        ref={textFieldRef}
        value={value.map((item) => item.name).join(", ")}
        renderValue={(selected) => (
          <span>{selected.map((e) => e.name).join(", ")}</span>
        )}
        onChange={onChange}
        onClick={handleOpen}
        placeholder={placeholder}
        InputProps={{
          disableUnderline: true,
          endAdornment: (
            isOpen 
              ? (<div class={css.dropDownListExpandedUp}></div>) 
              : (<div class={css.dropDownListExpandedDown}></div>)
          ),
          readOnly: true
        }}
        slots={slots}
        slotsProps={slots}
        MenuProps={MenuProps}
        inputProps={{ sx: { "&:focus": { backgroundColor: "transparent" } } }}
      />
      {isOpen && (
        <ClickAwayListener onClickAway={handleClose}>
          <Popper
            open={isOpen}
            anchorEl={textFieldRef.current}
            placement="bottom-start"
            disablePortal={false}
            transition={false}
            container={textFieldRef.current?.parentElement}
            modifiers={[
              { name: "preventOverflow", options: { boundary: "viewport" } },
              { name: "offset", options: { offset: [0, 8] } },
              { name: "flip", options: { fallbackPlacements: ["bottom-start", "top-start"] } },
            ]}
            sx={{
              position: "fixed !important",
              zIndex: "1400",
              margin: "3px 0 3px !important",
              "& .MuiPaper-root": { backgroundColor: "#512068" }
            }}
          >
            <Paper
              style={{
                width: anchorEl
                  ? anchorEl.offsetWidth
                  : 'auto', maxHeight: 300,
                overflowY: "auto",
                padding: "0px 0px 3px 0",
              }}
              sx={{/*
                "& .MuiPaper-root::-webkit-scrollbar": {
                  width: "14px",
                },
                "& .MuiPaper-root::-webkit-scrollbar-thumb": {
                  border: "6px solid transparent",
                  backgroundClip: "padding-box",
                  borderRadius: "9999px",
                  backgroundColor: "rgb(131, 64, 155)"
                }*/
              }}
            >
              <div style={{
                position: "sticky",
                top: 0,
                background: "#83409b",
                zIndex: 1500,
                padding: "8px",
                paddingRight: "15px",
              }}>
                <TextField
                  variant="outlined"
                  placeholder="Поиск издателя..."
                  fullWidth
                  inputRef={searchFieldRef}
                  value={search}
                  onChange={handleSearchChange}
                  container={textFieldRef.current?.parentElement}
                  sx={{
                    padding: "5px",
                    width: "100%",
                    height: "40px",
                    backgroundColor: "#83409b",
                    borderRadius: "34px",
                    "& .MuiInputBase-root": { height: "100%" },
                    "& .MuiOutlinedInput-input": { borderRadius: "34px !important", cursor: "text !important" },
                    "& .MuiOutlinedInput-notchedOutline": { borderRadius: "34px !important", borderColor: "rgba(0, 0, 0, 0.0)" }
                  }}
                />
              </div>
              {filteredOptions?.map((item) => (
                <MenuItem
                  key={item.id}
                  value={item}
                  dense
                  sx={{
                    padding: "3px 0",
                    "&.Mui-selected span ": { color: "rgb(255, 255, 255)" },
                  }}
                  container={textFieldRef.current?.parentElement}
                  onClick={() => handleSelect(item)}
                  // ref={menuItemRef}
                >
                  <ListItemText
                    sx={{
                      paddingLeft: "15px",
                      "& div": {
                        [`&.${optionUnstyledClasses.selected}`]: {
                          backgroundColor: "none",
                          color: "#FFFFFF",
                        },

                        [`&.${optionUnstyledClasses.highlighted}`]: {
                          backgroundcolor: "none",
                          color: "#FFFFFF",
                        },

                        [`&.${optionUnstyledClasses.highlighted}.${optionUnstyledClasses.selected}`]:
                        {
                          backgroundColor: "none",
                          color: "#FFFFFF",
                        },

                        [`&.${optionUnstyledClasses.disabled}`]: {
                          color: grey[400],
                        },

                        [`&:hover:not(.${optionUnstyledClasses.disabled})`]: {
                          backgroundColor: "none",
                          color: "#FFFFFF",
                        },
                      },
                    }}
                    style={{ color: item.color || "#B3B3B3" }}
                    primary={<div>{item.name}</div>}
                  />
                  <Checkbox
                    icon={<div></div>}
                    checkedIcon={
                      <div
                        style={{ width: "12px", height: "12px" }}
                        className={checkBoxCss.checked}
                      ></div>
                    }
                    // anchorEl={menuItemRef}
                    className={checkBoxCss.wrapper}
                    sx={{
                      margin: "0px 15px",
                      width: "20px",
                      height: "20px",
                      padding: "0px"
                    }}
                    checked={value.some((elem) => elem.id === item.id)}
                  />
                </MenuItem>
              ))}
            </Paper>
          </Popper>
        </ClickAwayListener>)}
    </FormControl>
  );
};

export default SelectWithSearchingTextField;