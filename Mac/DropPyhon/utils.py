def check_n(input_str: str) -> tuple[bool, str]:
    prefix = "NNN"

    if input_str and input_str.lower().startswith(prefix.lower()):
        value = input_str[len(prefix):]
        return True, value

    return False, input_str or ""

