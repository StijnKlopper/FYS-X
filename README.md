# FYS-X
Repository for a game made by Team FYS-X as the project during the thematic semester for Software Engineering.

_Made By_<br>
Tarik Yıldırım <br>
Stijn Klopper <br>
Rick den Otter <br>
Stan van Weringh <br>

## Best practices
Under this header, the best practices to ensure good teamwork will be explained.

### GIT/GitHub
There are two main branches:

- master, to keep the main code, updated the least often and code changes must be approved by all other team members.
- dev, to develop on, for every change made to the project.

#### Developing new functionality
New functionality will start on the dev branch. The project member will create a branch using the following naming scheme:
```
git checkout -b dev-{issue number}:{issue description}

Example for a pull request that changes the README.md:
git checkout -b dev-12:update-readme

Example for a pull request that fixes a bug about chunks not generating:
git checkout -b dev-1:chunks-not-generating

Example for a pull request that adds new functionality, adding biomes:
git checkout -b dev-2:biome-generation
```

After creating this branch and finishing the functionality, the project member will do the following to push the changes to the branch:
```
git add .

git commit -m {commit message}

(if branch is new)
git push --set-upstream origin {branch name}

(else)
git push
```

Then, the project member will go to GitHub, go to their branch in the branch selector and open a merge request. To solve a connected issue, make sure that '{closes/fixes/resolves} #{issue number}' is in the comment, like:
```
Fixes #12
``` 

Afterwards, the other three project members will add their comments and approve of the merge request.