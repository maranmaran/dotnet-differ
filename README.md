# Differ.DotNet

![DotnetDiffer Banner](https://raw.githubusercontent.com/maranmaran/differ-dotnet/main/banner.png)

Differ.DotNet is diffing library for custom types and objects.

Get a list of differences of your instances quickly with flexibility to specify custom property names, what to keep or ignore in your difference and make your change observation features easier.

# Installation

[![NuGet](https://img.shields.io/nuget/v/Differ.DotNet.svg)](https://www.nuget.org/packages/Differ.DotNet)

# Usage

## Retrieve differences:

```cs
record Car(string Model, string Color, int Year);

Car car1 = new Car("Toyota Camry", "Blue", 2022);
Car car2 = new Car("Ford Mustang", "Red", 2023);

IEnumerable<Difference> carDiff = DifferDotNet.Diff(car1, car2);
```

Output:

```
[
  {
    "fullPath": "color",
    "fieldPath": "",
    "fieldName": "color",
    "leftValue": "Blue",
    "rightValue": "Red"
  },
  {
    "fullPath": "model",
    "fieldPath": "",
    "fieldName": "model",
    "leftValue": "Toyota Camry",
    "rightValue": "Ford Mustang"
  },
  {
    "fullPath": "year",
    "fieldPath": "",
    "fieldName": "year",
    "leftValue": 2022,
    "rightValue": 2023
  }
]
```

## KeepInDiff attribute

Keep root and child values even if there is no diff

```cs
class Car([property:KeepInDiff]string Model);

Car car1 = new Car("Toyota");
Car car2 = new Car("Toyota");

IEnumerable<Difference> carDiff = DifferDotNet.Diff(car1, car2);
```

Output

```
[
  {
    "fullPath": "model",
    "fieldPath": "",
    "fieldName": "model",
    "leftValue": "Toyota",
    "rightValue": "Toyota"
  }
]
```

## IgnoreInDiff attribute

Ignores root and child values

```cs
class Car([property:IgnoreInDiff]string Model);

Car car1 = new Car("Toyota");
Car car2 = new Car("Toyota");

// Empty diff
IEnumerable<Difference> carDiff = DifferDotNet.Diff(car1, car2);
```

# Keep and Ignore attribute strength:

More nested attributes have bigger strength than parent ones.
This means that if you define `IgnoreInDiff` on root property, but `KeepInDiff` child property or **reverse**, expect that child property attribute to override parent one.

## DiffPropertyName attribute

Applies custom names into `Difference.CustomFullPath`, `Difference.CustomFieldPath`, `Difference.CustomFieldName` props

```cs
class Car([property:DiffPropertyName("Make")]string Model);

Car car1 = new Car("Toyota");
Car car2 = new Car("Ford");

IEnumerable<Difference> carDiff = DifferDotNet.Diff(car1, car2);
```

Output

```
[
  {
    "fullPath": "model",
    "fieldPath": "",
    "fieldName": "model",
    "leftValue": "Toyota",
    "rightValue": "Ford",
    "customFullPath": "Make",
    "customFieldPath": "",
    "customFieldName": "Make"
  }
]
```

# Full demo

Combine and enjoy:

```cs
    public class Car
    {
        public string Model { get; set; }

        [IgnoreInDiff]
        public string Color { get; set; }

        public int Year { get; set; }

        public List<Accessory> Accessories { get; set; }

        [IgnoreInDiff]
        public List<string> Features { get; set; }

        [JsonIgnore]
        [DiffPropertyName("features")]
        public string FeaturesFlag => string.Join(", ", Features);

        public Car(string model, string color, int year)
        {
            Model = model;
            Color = color;
            Year = year;
            Accessories = new List<Accessory>();
            Features = new List<string>();
        }
    }

    public class Accessory
    {
        [KeepInDiff]
        public string Name { get; set; }

        public decimal Price { get; set; }
    }

    Car car1 = new Car("Toyota Camry", "Blue", 2022);
    car1.Accessories.Add(new Accessory { Name = "Floor Mats", Price = 50.99m });
    car1.Accessories.Add(new Accessory { Name = "Roof Rack", Price = 150.99m });
    car1.Features.Add("GPS Navigation");
    car1.Features.Add("Backup Camera");

    Car car2 = new Car("Honda Civic", "Silver", 2023);
    car2.Accessories.Add(new Accessory { Name = "Floor Mats", Price = 80.99m });
    car2.Accessories.Add(new Accessory { Name = "Roof Rack", Price = 200.50m });
    car2.Features.Add("Sunroof");
    car2.Features.Add("Lane Departure Warning");

    IEnumerable<Difference> carDiff = DifferDotNet.Diff(car1, car2);
```

Output:

```
[
  {
    "fullPath": "accessories.0.name",
    "fieldPath": "accessories.0",
    "fieldName": "name",
    "leftValue": "Floor Mats",
    "rightValue": "Floor Mats"
  },
  {
    "fullPath": "accessories.0.price",
    "fieldPath": "accessories.0",
    "fieldName": "price",
    "leftValue": 50.99,
    "rightValue": 80.99
  },
  {
    "fullPath": "accessories.1.name",
    "fieldPath": "accessories.1",
    "fieldName": "name",
    "leftValue": "Roof Rack",
    "rightValue": "Roof Rack"
  },
  {
    "fullPath": "accessories.1.price",
    "fieldPath": "accessories.1",
    "fieldName": "price",
    "leftValue": 150.99,
    "rightValue": 200.50
  },
  {
    "fullPath": "featuresFlag",
    "fieldPath": "",
    "fieldName": "featuresFlag",
    "leftValue": "GPS Navigation, Backup Camera",
    "rightValue": "Sunroof, Lane Departure Warning",
    "customFullPath": "features",
    "customFieldPath": "",
    "customFieldName": "features"
  },
  {
    "fullPath": "model",
    "fieldPath": "",
    "fieldName": "model",
    "leftValue": "Toyota Camry",
    "rightValue": "Honda Civic"
  },
  {
    "fullPath": "year",
    "fieldPath": "",
    "fieldName": "year",
    "leftValue": 2022,
    "rightValue": 2023
  }
]
```

## License

See [LICENSE](https://github.com/maranmaran/differ-dotnet/blob/main/LICENSE).

## Copyright

Copyright (c) 2023 Marko Urh and other authors.
