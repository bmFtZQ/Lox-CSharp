# lox-csharp

A C# implementation of the tree-walker lox interpreter from
[Crafting Interpreters][1], with some additional features.

[More information about the Lox language][2]

## Differences from JLox:

* There are augmented assignment operators for the following `+ - * /`. So
  `i += 1` will be converted to `i = i + 1`.
* `+` operator will convert values to strings if at least one string is present.
  For example `"str" + 5` will return `"str5"`.
* Functions can be created in expressions using `fun (<params>) {<body>}`:
  ```
  var function = fun (a, b) { return a + b; };
  ```
* Indexer syntax for getting/setting instance fields dynamically, for example:
  `instance["a"]` would get the field `a` on object `instance`.
* Classes have static methods and fields, with static methods declared using
  `class` keyword:
  ```
  class Example {
    // Static initializer, will be run automatically, cannot contain parameters.
    class init() {
      this.field = 0; // 'this' refers to the class within static method.
    }
  }
  ```

  Static methods are also inherited (and can be overridden) from a super class:
  ```
  class A {
    class method() {
      print "class A";
    }
  }

  class B < A {}

  B.method(); // prints "class A"
  ```

### Arrays

This version of Lox also supports arrays. They can be created using the `Array`
global class or using an array expression `[...]` with a comma separated list of
values.

Array values can be assigned or get using the indexer syntax `array[index]`.
Though the index value must be a number, string indexes will get a field.

```
var arr = [1, 2, 3];

for (var i = 0; i < arr.length(); i += 1) {
  print arr[i];
}
```

### Built-ins

#### Classes

* Console
  * `class readLine()` - Read a line of user input from console.
  * `class writeLine(v)` - Write a value `v` to the console, with a new line.
  * `class write(v)` - Write a value `v` to the console, no new lines.
* Array
  * `Array(size)` - Make a new array with `size` length, elements
    initialized to `nil`.
  * `get(i)` - Get a single value from array at index `i`.
  * `set(i, v)` - Set a single value `v` at index `i`.
  * `length()` - Get the length of the array.
  * `fill(v)` - Fill array in place with value `v`, returns the array.
  * `foreach(function)` - Supply a function that will be run against every
    element, with arguments `item` and `index`.
* String
  * `class length(s)` - Get the length of a string in UTF-16 characters.
  * `class charAt(s, i)` - Get the specified character of string, returned as a
    new string.
  * `class charCodeAt(s, i)` - Get the UTF-16 character code of specified
    character index, returned as a number.
* Math
  * `class mod(a, b)` - Get modulo of two numbers.
  * `class round(v, d)` - Round number `v` to `d` decimal places.

#### Functions

* `clock()` - Get the current time in milliseconds.
* `string(v)` - Convert a value to a string.
* `number(v)` - Convert a value to a number, or nil if not possible.
* `typeOf(v)` - Get type code of a value, returns:
  * `"nil"` 
  * `"boolean"`
  * `"string"`
  * `"number"`
  * `"class"`
  * `"instance"`
  * `"function"`
* `is(v, t)` - Test that value `v` is of type `t`, `t` can be a type code or a
  class, if `t` is a class, will only return true if `v` is an instance.
* `fields(i)` - Get all fields of an instance, returned as an array containing
  the names of each field as a string.
* `methods(i)` - Get all methods of an instance as an array of method names.
* `hasField(i, f)` - Test that an instance `i` has field `f`.
* `hasMethod(i, m)` - Test that an instance `i` has field `f`.

[1]: https://craftinginterpreters.com
[2]: https://craftinginterpreters.com/the-lox-language.html
