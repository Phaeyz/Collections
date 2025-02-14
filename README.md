# Phaeyz

Phaeyz is a set of libraries created and polished over time for use with other projects, and made available here for convenience.

All Phaeyz libraries may be found [here](https://github.com/Phaeyz).

# Phaeyz.Collections

API documentation for **Phaeyz.Collections** library is [here](https://github.com/Phaeyz/Collections/blob/main/docs/Phaeyz.Collections.md).

This library contains collection classes which are useful for other projects, such as [Phaeyz.Exif](https://github.com/Phaeyz/Exif). Here are some highlights.

## KeyedCollection

```C#
// Both ICollection<KeyValuePair<K, V>> and IDictionary<K, V> are implemented
var collection = new KeyedCollection<int, string>(); // Essentially, an ordered dictionary
collection[1] = "one"; // Elements are added in order by default
collection[3] = "three";
var index = collection.IndexOf(3); // Indexes may be fetched
collection.Insert(index, new KeyValuePair<int, string>(2, "two")); // Elements can be inserted
foreach (KeyValuePair<int, string> kvp in collection)
{
    Console.WriteLine($"{kvp.Key} = {kvp.Value}"); // Elements always iterated in order
}
string value = collection[2]; // Keys are used just like a dictionary
```

# Licensing

This project is licensed under GNU General Public License v3.0, which means you can use it for personal or educational purposes for free. However, donations are always encouraged to support the ongoing development of adding new features and resolving issues.

If you plan to use this code for commercial purposes or within an organization, we kindly ask for a donation to support the project's development. Any reasonably sized donation amount which reflects the perceived value of using Phaeyz in your product or service is accepted.

## Donation Options

There are several ways to support Phaeyz with a donation. Perhaps the best way is to use Patreon so that recurring small donations continue to support the development of Phaeyz.

- **Patreon**: [https://www.patreon.com/phaeyz](https://www.patreon.com/phaeyz)
- **Bitcoin**: Send funds to address: ```bc1qdzdahz8d7jkje09fg7s7e8xedjsxm6kfhvsgsw```
- **PayPal**: Send funds to ```phaeyz@pm.me``` ([directions](https://www.paypal.com/us/cshelp/article/how-do-i-send-money-help293))

Your support is greatly appreciated and helps me continue to improve and maintain Phaeyz!