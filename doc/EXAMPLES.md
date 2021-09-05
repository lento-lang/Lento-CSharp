# Examples

Lento is an expression based language, everything is an expression.

### Basic Arithmetic

```rust
4 * -3 + 12 - -3 + 4 * 15
// 63
```

### Variables

```rust
x = 5
x * 15 // 75
```

### Booleans

```rust
true != false // true
!true // false
!((true && false) | true) // false
```

### Lists

```rust
ls = [1, 2, 9, 4, 5]
ls // [1, 2, 9, 4, 5]

ls2 = ls + [2, 4, 6, 8]
ls2 // [1, 2, 9, 4, 5, 2, 4, 6, 8]
```

### Characters and Strings

```rust
capA = 'A'
cool = "Cool!"

'A' > 'B' // false

"Hello" + '!' // "Hello!"

"Hello, " + "world!" // "Hello, world!"
```

### Special Characters

```rust
'\u0007' == '\a' // true
"\t" + "Hi" + '\n' // \tHi\n
```

### Code Blocks

```rust
x = 1
blockFn = {
	y = 2;
	z = 3;
	x + y + z;
} // 6

{
	a = 12; // Block scoped
}
a; // Undefined variable 'a'

```

### Functions

```rust
f(int x) = x * 2
f(30) // 60

apply_twice any f, int x = f(f(x)) // Ommit parenthesis surrounding parameters
apply_twice(f, 5) // 20
```

### Function variations

```rust
f() = 2
f int x = x // Ommit parenthesis surrounding parameters

f() // 2
f(6) // 6

f int x = x * 2
// Function already contains a definition matching: f int x

F(int a, int b) = a * b
F F 2 3 3     // 18
F F(2, 3) 3   // 18
F(F(2, 3), 3) // 18

F(Tuple<1> t) = t * 2
F#(5)   // 10
F(#(5, 2)) // No function variation matches the given argument signature 'F(Tuple<2>)'
```

### Small Standard Library

```rust
print(3, 5) // 3, 5
println("Hello, world!")
```
