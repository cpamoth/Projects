<!DOCTYPE html>
<html>
<body>

<?php 
// ForLoops Examples 
for ($x = 0; $x <= 20; $x++) {
  echo "The number is: $x <br>";
}

$colors = array("red", "green", "blue", "yellow", "purple", "orange"); 

foreach ($colors as $value) {
  echo "$value <br>";
}

// WhileLoops Examples
$x = 1; 

while($x <= 20) {
    echo "The number is: $x <br>";
    $x++;
} 

$x = 1;

do {
    echo "The number is: $x <br>";
    $x++;
} while ($x <= 10);
?>  

</body>
</html>