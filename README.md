# Real-Time Chat in C# WPF


## Setup a Realtime Database

So first just make a realtime database and then get your link, should look something like this: https://tutorial-e2f94-default-rtdb.firebaseio.com/

Also change your rules to this:
```
{
  "rules": {
    ".read": true,
    ".write": true,
    
    "users": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid"
      }
    },
    
    "messages": {
      ".read": true,
      ".write": "auth != null",
      ".indexOn": ["timestamp"]
    }
  }
}
```

I will make a tutorial video just for you children that dont know how to setup Google Firebase:
![Video](https://streamable.com/e/uh6v5b)


# Alright Children now enjoy and you can do whatever you please with this devious source !!!!
